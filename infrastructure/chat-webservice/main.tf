module "chat_webservice" {
  source = "../shared/modules/k8s-helm-deployment"

  namespace        = var.namespace
  app_name         = var.app_name
  helm_chart_path  = "${path.module}/helm"
  create_namespace = false

  helm_set_values = {
    "app.deploymentTimestamp" = timestamp()
  }

  helm_set_sensitive_values = {
    "app.secret.data.DiscordBotToken"                              = var.discord_bot_token
    "app.secret.data.OpenAIApiKey"                                 = var.openai_api_key
    "app.secret.data.TableStorageConnectionString"                 = var.azure_storage_connection_string
    "app.envFromSecret.Discord__DiscordBotToken.secretName"        = var.app_name
    "app.envFromSecret.Discord__DiscordBotToken.key"               = "DiscordBotToken"
    "app.envFromSecret.OpenAI__OpenAIApiKey.secretName"            = var.app_name
    "app.envFromSecret.OpenAI__OpenAIApiKey.key"                   = "OpenAIApiKey"
    "app.envFromSecret.ConnectionStrings__TableStorage.secretName" = var.app_name
    "app.envFromSecret.ConnectionStrings__TableStorage.key"        = "TableStorageConnectionString"
  }
}
