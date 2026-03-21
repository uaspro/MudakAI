module "tts_python_api" {
  source = "../shared/modules/k8s-helm-deployment"

  namespace        = var.namespace
  app_name         = var.app_name
  helm_chart_path  = "${path.module}/helm"
  create_namespace = false

  helm_set_values = {
    "app.deploymentTimestamp" = timestamp()
  }

  helm_set_sensitive_values = {}
}
