module "voice_functions" {
  source = "../shared/modules/k8s-helm-deployment"

  namespace        = var.namespace
  app_name         = var.app_name
  helm_chart_path  = "${path.module}/helm"
  create_namespace = false

  helm_set_values = {
    "app.deploymentTimestamp" = timestamp()
  }

  helm_set_sensitive_values = {
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
