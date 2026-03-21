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
  default     = "tts-api"
}

variable "azure_service_bus_connection_string" {
  description = "Azure Service Bus connection string"
  type        = string
  sensitive   = true
}
