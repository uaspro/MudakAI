variable "kubernetes_context" {
  description = "Kubernetes context to use"
  type        = string
  default     = "t3-k3s"
}

variable "dapr_version" {
  description = "Dapr Helm chart version"
  type        = string
  default     = "1.14"
}
