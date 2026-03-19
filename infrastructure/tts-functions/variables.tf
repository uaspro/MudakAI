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
  default     = "tts-functions"
}

variable "registry_password" {
  description = "Private container registry password"
  type        = string
  sensitive   = true
}

variable "azure_storage_connection_string" {
  description = "Azure Storage connection string"
  type        = string
  sensitive   = true
}

variable "azure_service_bus_connection_string" {
  description = "Azure Service Bus connection string"
  type        = string
  sensitive   = true
}

variable "openai_api_key" {
  description = "OpenAI API key"
  type        = string
  sensitive   = true
}
