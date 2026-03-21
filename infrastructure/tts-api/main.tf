module "tts_api" {
  source = "../shared/modules/k8s-helm-deployment"

  namespace        = var.namespace
  app_name         = var.app_name
  helm_chart_path  = "${path.module}/helm"
  create_namespace = false

  helm_set_values = {
    "app.deploymentTimestamp" = timestamp()
  }

  helm_set_sensitive_values = {
    "app.secret.data.ServiceBusConnectionString"                 = var.azure_service_bus_connection_string
    "app.envFromSecret.ConnectionStrings__ServiceBus.secretName" = var.app_name
    "app.envFromSecret.ConnectionStrings__ServiceBus.key"        = "ServiceBusConnectionString"
  }
}
