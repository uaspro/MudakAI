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
