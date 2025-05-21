# Elix Backend 

## Prérequis

Docker et Docker Compose installés.

Les services : 
- une web API en .NET 9.0
- une BDD Postgresql

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
