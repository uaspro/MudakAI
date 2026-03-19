locals {
  registry_dockerconfig = base64encode(jsonencode({
    auths = {
      "registry-docker-registry.container-registry.svc.cluster.local:5000" = {
        username = "admin"
        password = var.registry_password
        auth     = base64encode("admin:${var.registry_password}")
      }
    }
  }))
}

module "tts_functions" {
  source = "../shared/modules/k8s-helm-deployment"

  namespace       = var.namespace
  app_name        = var.app_name
  helm_chart_path = "${path.module}/helm"

  helm_set_values = {
    "app.deploymentTimestamp"        = timestamp()
    "app.imagePullSecrets[0].name"   = "registry-credentials"
    "app.imagePullSecrets[0].create" = "true"
  }

  helm_set_sensitive_values = {
    "app.imagePullSecrets[0].dockerconfigjson"           = local.registry_dockerconfig
    "app.secret.data.AzureWebJobsStorage"                = var.azure_storage_connection_string
    "app.secret.data.BlobStorage"                        = var.azure_storage_connection_string
    "app.secret.data.ServiceBusListen"                   = var.azure_service_bus_connection_string
    "app.secret.data.ServiceBusSend"                     = var.azure_service_bus_connection_string
    "app.secret.data.OpenAIApiKey"                       = var.openai_api_key
    "app.envFromSecret.AzureWebJobsStorage.secretName"   = var.app_name
    "app.envFromSecret.AzureWebJobsStorage.key"          = "AzureWebJobsStorage"
    "app.envFromSecret.BlobStorage.secretName"           = var.app_name
    "app.envFromSecret.BlobStorage.key"                  = "BlobStorage"
    "app.envFromSecret.ServiceBusListen.secretName"      = var.app_name
    "app.envFromSecret.ServiceBusListen.key"             = "ServiceBusListen"
    "app.envFromSecret.ServiceBusSend.secretName"        = var.app_name
    "app.envFromSecret.ServiceBusSend.key"               = "ServiceBusSend"
    "app.envFromSecret.OpenAI__OpenAIApiKey.secretName"  = var.app_name
    "app.envFromSecret.OpenAI__OpenAIApiKey.key"         = "OpenAIApiKey"
  }
}
