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

module "chat_webservice" {
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
    "app.imagePullSecrets[0].dockerconfigjson"                     = local.registry_dockerconfig
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
