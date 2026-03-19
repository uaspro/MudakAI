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

module "voice_functions" {
  source = "../shared/modules/k8s-helm-deployment"

  namespace        = var.namespace
  app_name         = var.app_name
  helm_chart_path  = "${path.module}/helm"
  create_namespace = false

  helm_set_values = {
    "app.deploymentTimestamp"        = timestamp()
    "app.imagePullSecrets[0].name"   = "registry-credentials"
    "app.imagePullSecrets[0].create" = "true"
  }

  helm_set_sensitive_values = {
    "app.imagePullSecrets[0].dockerconfigjson"          = local.registry_dockerconfig
    "app.secret.data.AzureWebJobsStorage"               = var.azure_storage_connection_string
    "app.secret.data.BlobStorage"                        = var.azure_storage_connection_string
    "app.secret.data.ServiceBus"                         = var.azure_service_bus_connection_string
    "app.secret.data.DiscordBotToken"                    = var.discord_bot_token
    "app.envFromSecret.AzureWebJobsStorage.secretName"   = var.app_name
    "app.envFromSecret.AzureWebJobsStorage.key"          = "AzureWebJobsStorage"
    "app.envFromSecret.BlobStorage.secretName"            = var.app_name
    "app.envFromSecret.BlobStorage.key"                  = "BlobStorage"
    "app.envFromSecret.ServiceBus.secretName"             = var.app_name
    "app.envFromSecret.ServiceBus.key"                   = "ServiceBus"
    "app.envFromSecret.DiscordBotToken.secretName"        = var.app_name
    "app.envFromSecret.DiscordBotToken.key"              = "DiscordBotToken"
  }
}
