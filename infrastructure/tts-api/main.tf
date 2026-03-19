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

module "tts_api" {
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
    "app.imagePullSecrets[0].dockerconfigjson"                   = local.registry_dockerconfig
    "app.secret.data.ServiceBusConnectionString"                 = var.azure_service_bus_connection_string
    "app.envFromSecret.ConnectionStrings__ServiceBus.secretName" = var.app_name
    "app.envFromSecret.ConnectionStrings__ServiceBus.key"        = "ServiceBusConnectionString"
  }
}
