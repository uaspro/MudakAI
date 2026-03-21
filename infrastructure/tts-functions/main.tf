module "tts_functions" {
  source = "../shared/modules/k8s-helm-deployment"

  namespace        = var.namespace
  app_name         = var.app_name
  helm_chart_path  = "${path.module}/helm"
  create_namespace = false

  helm_set_values = {
    "app.deploymentTimestamp" = timestamp()
  }

  helm_set_sensitive_values = {
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
