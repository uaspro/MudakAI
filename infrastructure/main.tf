locals {
  registry_host = "registry-docker-registry.container-registry.svc.cluster.local:5000"
}

resource "kubernetes_namespace" "this" {
  metadata {
    name = var.namespace
  }
}

resource "kubernetes_secret" "registry_credentials" {
  metadata {
    name      = "registry-credentials"
    namespace = kubernetes_namespace.this.metadata[0].name
  }

  type = "kubernetes.io/dockerconfigjson"
  data = {
    ".dockerconfigjson" = jsonencode({
      auths = {
        (local.registry_host) = {
          username = "admin"
          password = var.registry_password
          auth     = base64encode("admin:${var.registry_password}")
        }
      }
    })
  }
}

resource "helm_release" "dapr" {
  name             = "dapr"
  namespace        = "dapr-system"
  create_namespace = true
  repository       = "https://dapr.github.io/helm-charts/"
  chart            = "dapr"
  version          = var.dapr_version
  wait             = true
  timeout          = 300

  set {
    name  = "global.ha.enabled"
    value = "false"
  }
}

module "tts_api" {
  source = "../../pi-shared-infrastructure/library/modules/k8s-helm-deployment"

  namespace        = kubernetes_namespace.this.metadata[0].name
  app_name         = "tts-api"
  helm_chart_path  = "${path.module}/helm/tts-api"
  create_namespace = false

  force_update = false

  helm_set_sensitive_values = {
    "app.secret.data.ServiceBusConnectionString" = var.azure_service_bus_connection_string
  }
  depends_on = [helm_release.dapr, kubernetes_secret.registry_credentials]
}

module "tts_functions" {
  source = "../../pi-shared-infrastructure/library/modules/k8s-helm-deployment"

  namespace        = kubernetes_namespace.this.metadata[0].name
  app_name         = "tts-functions"
  helm_chart_path  = "${path.module}/helm/tts-functions"
  create_namespace = false

  force_update = false

  helm_set_sensitive_values = {
    "app.secret.data.AzureWebJobsStorage" = var.azure_storage_connection_string
    "app.secret.data.BlobStorage"         = var.azure_storage_connection_string
    "app.secret.data.ServiceBusListen"    = var.azure_service_bus_connection_string
    "app.secret.data.ServiceBusSend"      = var.azure_service_bus_connection_string
    "app.secret.data.OpenAIApiKey"        = var.openai_api_key
  }
  depends_on = [helm_release.dapr, kubernetes_secret.registry_credentials]
}

module "voice_functions" {
  source = "../../pi-shared-infrastructure/library/modules/k8s-helm-deployment"

  namespace        = kubernetes_namespace.this.metadata[0].name
  app_name         = "voice-functions"
  helm_chart_path  = "${path.module}/helm/voice-functions"
  create_namespace = false

  force_update = false

  helm_set_sensitive_values = {
    "app.secret.data.AzureWebJobsStorage" = var.azure_storage_connection_string
    "app.secret.data.BlobStorage"         = var.azure_storage_connection_string
    "app.secret.data.ServiceBus"          = var.azure_service_bus_connection_string
    "app.secret.data.DiscordBotToken"     = var.discord_bot_token
  }
  depends_on = [helm_release.dapr, kubernetes_secret.registry_credentials]
}

module "chat_webservice" {
  source = "../../pi-shared-infrastructure/library/modules/k8s-helm-deployment"

  namespace        = kubernetes_namespace.this.metadata[0].name
  app_name         = "chat-webservice"
  helm_chart_path  = "${path.module}/helm/chat-webservice"
  create_namespace = false

  force_update = false

  helm_set_sensitive_values = {
    "app.secret.data.DiscordBotToken"              = var.discord_bot_token
    "app.secret.data.OpenAIApiKey"                 = var.openai_api_key
    "app.secret.data.TableStorageConnectionString" = var.azure_storage_connection_string
  }
  depends_on = [helm_release.dapr, kubernetes_secret.registry_credentials, module.tts_api, module.tts_functions, module.voice_functions]
}
