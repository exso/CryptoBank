#!/bin/bash

# Запускаем Terraform
terraform apply -auto-approve

# Используем terraform output для получения значений
backend_ip=$(terraform output -raw backend_ip)
backend_public_ip=$(terraform output -raw backend_public_ip)
database_ip=$(terraform output -raw database_ip)
database_public_ip=$(terraform output -raw database_public_ip)
database_volume_id=$(terraform output -raw database_volume_id)
frontend_public_ip=$(terraform output -raw frontend_public_ip)

# Создаем файл inventory.ini
echo "" > ../ansible/inventory.ini
{
  echo "[frontend]"
  echo "$frontend_public_ip"
  echo ""

  echo "[backend]"
  echo "$backend_public_ip"
  echo ""

  echo "[database]"
  echo "$database_public_ip"
  echo ""

  echo "[database:vars]"
  echo "database_ip=$database_ip"
  echo "backend_ip=$backend_ip"
  echo "database_volume_id=$database_volume_id"
  echo "postgres_user=asus"
  echo "postgres_password=asus"
  echo ""
} >> ../ansible/inventory.ini

echo "Inventory file generated successfully."