variable "hcloud_token" {
  type = string
  sensitive = true
}

variable "ssh_key_fingerprint" {
  type = string
  default = "96:9d:48:44:48:d5:4e:45:91:c2:bc:1c:e8:3f:27:bf"
}