variable "kubernetes_context" {
  description = "Kubernetes context to use"
  type        = string
  default     = "t3-k3s"
}

variable "namespace" {
  description = "Kubernetes namespace"
  type        = string
  default     = "mudakai"
}

variable "app_name" {
  description = "Application name"
  type        = string
  default     = "tts-python-api"
}

variable "registry_password" {
  description = "Private container registry password"
  type        = string
  sensitive   = true
}
