"""Protect the service identities and runtime wiring used by Discord and Dapr.

Run with: python -m unittest discover -s infrastructure/tests -v
Requires Helm and PyYAML; does not contact the cluster or use real secrets.
"""

import json
from pathlib import Path
import subprocess
import unittest

import yaml


INFRASTRUCTURE = Path(__file__).resolve().parents[1]
CONTRACTS = {
    "chat-webservice": {
        "image": "mudakai-chat", "port": 8080, "dapr": "mudakaichat-webservice",
        "secrets": {
            "ConnectionStrings__TableStorage": "TableStorageConnectionString",
            "Discord__DiscordBotToken": "DiscordBotToken",
            "OpenAI__OpenAIApiKey": "OpenAIApiKey",
        },
    },
    "tts-api": {
        "image": "mudakai-tts-api", "port": 8080, "dapr": "mudakaitexttospeech-api",
        "secrets": {"ConnectionStrings__ServiceBus": "ServiceBusConnectionString"},
    },
    "tts-functions": {
        "image": "mudakai-tts-functions", "port": 80, "dapr": None,
        "secrets": {
            "AzureWebJobsStorage": "AzureWebJobsStorage", "BlobStorage": "BlobStorage",
            "ServiceBusListen": "ServiceBusListen", "ServiceBusSend": "ServiceBusSend",
            "OpenAI__OpenAIApiKey": "OpenAIApiKey",
        },
    },
    "voice-functions": {
        "image": "mudakai-voice-functions", "port": 80, "dapr": "mudakaivoice-functions",
        "secrets": {
            "AzureWebJobsStorage": "AzureWebJobsStorage", "BlobStorage": "BlobStorage",
            "ServiceBus": "ServiceBus", "DiscordBotToken": "DiscordBotToken",
        },
    },
}


class DeploymentCompatibilityTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.resources = {}
        for name in CONTRACTS:
            chart = INFRASTRUCTURE / "helm" / name
            subprocess.run(["helm", "dependency", "build", str(chart), "--skip-refresh"],
                           check=True, capture_output=True, text=True)
            rendered = subprocess.run(["helm", "template", name, str(chart), "-n", "mudakai"],
                                      check=True, capture_output=True, text=True).stdout
            cls.resources[name] = {
                doc["kind"]: doc for doc in yaml.safe_load_all(rendered) if doc
            }

    def test_existing_services_keep_their_names_and_immutable_selectors(self):
        for name, resources in self.resources.items():
            with self.subTest(service=name):
                labels = {"app.kubernetes.io/name": name, "app.kubernetes.io/instance": name}
                self.assertEqual(set(resources), {"Deployment", "Service", "ConfigMap", "Secret"})
                for resource in resources.values():
                    self.assertEqual(resource["metadata"]["name"], name)
                deployment = resources["Deployment"]["spec"]
                self.assertEqual(deployment["selector"]["matchLabels"], labels)
                self.assertEqual(deployment["template"]["metadata"]["labels"], labels)
                self.assertEqual(resources["Service"]["spec"]["selector"], labels)

    def test_dapr_callers_keep_their_app_ids_and_the_speech_worker_stays_independent(self):
        for name, contract in CONTRACTS.items():
            with self.subTest(service=name):
                annotations = self.resources[name]["Deployment"]["spec"]["template"]["metadata"]["annotations"]
                self.assertEqual(annotations["dapr.io/enabled"], "true" if contract["dapr"] else "false")
                if contract["dapr"]:
                    self.assertEqual(annotations["dapr.io/app-id"], contract["dapr"])
                    self.assertEqual(annotations["dapr.io/app-port"], str(contract["port"]))

    def test_secret_references_keep_the_configuration_keys_the_applications_read(self):
        for name, contract in CONTRACTS.items():
            with self.subTest(service=name):
                container = self.resources[name]["Deployment"]["spec"]["template"]["spec"]["containers"][0]
                refs = {env["name"]: env["valueFrom"]["secretKeyRef"]
                        for env in container["env"] if "valueFrom" in env}
                self.assertEqual(refs, {env: {"name": name, "key": key}
                                        for env, key in contract["secrets"].items()})
                self.assertEqual(set(self.resources[name]["Secret"]["stringData"]), set(contract["secrets"].values()))

    def test_image_ports_mounts_and_function_architectures_remain_compatible(self):
        for name, contract in CONTRACTS.items():
            with self.subTest(service=name):
                spec = self.resources[name]["Deployment"]["spec"]["template"]["spec"]
                container = spec["containers"][0]
                self.assertEqual(container["name"], "app")
                self.assertEqual(container["image"], "registry-docker-registry.container-registry.svc.cluster.local:5000/"
                                 + contract["image"] + ":latest")
                self.assertEqual(spec["imagePullSecrets"], [{"name": "registry-credentials"}])
                self.assertEqual(container["ports"][0]["containerPort"], contract["port"])
                service_port = self.resources[name]["Service"]["spec"]["ports"][0]
                self.assertEqual((service_port["port"], service_port["targetPort"]), (80, "http"))
                self.assertEqual(container["ports"][0]["name"], "http")
                is_function = name.endswith("functions")
                self.assertEqual(spec.get("nodeSelector", {}), {"kubernetes.io/arch": "amd64"} if is_function else {})
                mount = "/home/site/wwwroot/appsettings.json" if is_function else "/app/appsettings.json"
                self.assertEqual(container["volumeMounts"][0]["mountPath"], mount)

    def test_speech_queue_and_playback_contracts_stay_connected(self):
        configs = {name: json.loads(resources["ConfigMap"]["data"]["appsettings.json"])
                   for name, resources in self.resources.items()}
        self.assertEqual(configs["tts-api"]["TextToSpeechQueueName"], "text-to-speech")
        self.assertEqual(configs["tts-functions"]["ServiceBusPlaybackQueueName"], "playback")
        self.assertEqual(configs["tts-functions"]["OpenAI"]["TTSModelId"], "gpt-4o-mini-tts")
        for name in ("tts-functions", "voice-functions"):
            self.assertEqual(configs[name]["BlobStorageAudioContainerName"], "audio")
        self.assertEqual(configs["voice-functions"]["BlobStorageLocksContainerName"], "locks")


if __name__ == "__main__":
    unittest.main()
