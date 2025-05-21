# Database

**PostgreSQL**

Code first

## Quelques commandes 

**Créer migration** :
```bash
dotnet ef migrations add NOM_MIGRATION --project ElixBackend.Infrastructure --startup-project ElixBackend.API
```

**Jouer migration** :
```bash
dotnet ef database update --startup-project ElixBackend.API
```

**Delete migration** :
```bash
dotnet ef migrations remove --project ElixBackend.Infrastructure --startup-project ElixBackend.API
```



