# Elix Backend 

## Prérequis

Docker et Docker Compose installés.

Les services : 
- une web API en .NET 9.0
- une BDD Postgresql

## Initialisation du projet en local

### Variables d'environnements

```bash
cp .env.example .env
```

Entrer les valeurs des variables

### Lancer les services

```bash
docker-compose up --build -d
```

### Down les services

```bash
docker-compose down -v
```

### Entrer dans un container 

```bash
docker ps -a
```

Récupérer l'id du container

```bash
docker exec -it elix_api /bin/sh
```

[App back accessible](http://localhost:5000/swagger/index.html)

### Migrations

Créer une migration

```bash
dotnet ef migrations add NomDeLaMigration \
  --project ElixBackend.Infrastructure/ElixBackend.Infrastructure.csproj \
  --startup-project ElixBackend.API/ElixBackend.API.csproj
```

Supprimer la dernière migration

```bash
dotnet ef migrations remove \
  --project ElixBackend.Infrastructure/ElixBackend.Infrastructure.csproj \
  --startup-project ElixBackend.API/ElixBackend.API.csproj
```

Appliquer les migrations à la base

```bash
dotnet ef database update \
  --project ElixBackend.Infrastructure/ElixBackend.Infrastructure.csproj \
  --startup-project ElixBackend.API/ElixBackend.API.csproj
```

