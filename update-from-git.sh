#!/bin/bash
echo "🔄 Actualización inteligente desde Git..."

cd ~/Documentos/CNT/back-pg

# Descargar cambios
echo "📥 Descargando cambios desde Git..."
git pull origin main

# Detectar si Oracle existe y está corriendo
if docker ps | grep -q "oracle-xe"; then
    echo "✅ Oracle detectado (corriendo). Actualizando solo API..."
    docker compose up -d --build api
    echo "🌐 API actualizada: http://$(hostname -I | awk '{print $1}'):8080"
    
elif docker ps -a | grep -q "oracle-xe"; then
    echo "⚠️  Oracle detectado (detenido). Iniciando Oracle y API..."
    docker start oracle-xe
    sleep 10  # Esperar a que Oracle inicie
    docker compose up -d --build api
    
else
    echo "🔍 Oracle no detectado. Creando ambiente COMPLETO..."
    docker compose up -d --build
    echo "🎉 Ambiente NUEVO creado: API + Oracle"
fi

echo "✅ Actualización completada!"
docker compose ps
