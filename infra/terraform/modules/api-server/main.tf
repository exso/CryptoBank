resource "hcloud_server" "api_server" {
  name        = var.name
  server_type = var.server_type
  image       = var.image
  location    = var.location

  network {
    network_id = var.network_id
  }

  labels = {
    purpose = "api"
  }
}
