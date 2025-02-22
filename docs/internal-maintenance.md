# Internal Maintenance Notes

## MongoDB

### Importing one-off JSON files into MongoDB

First, mount the backup file into the container.

```sh
docker compose exec <container_name> mongoimport -d <database_name> -c <collection_name> --file <backup_file>
docker compose exec mongodb mongoimport -d PS2-collections -c outfit_war_round --file /tmp/backup-cols/outfit_war_round.json --jsonArray
```
