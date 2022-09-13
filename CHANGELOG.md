# Changelog

Date format: DD/MM/YYYY

## vNext

- Added the `skill`, `skill_category`, `skill_line` and `skill_set` collections.
- Added the `image` and `image_set_default` collections.

## 11/09/2022

- Removed `directive.objective_param_1` due to instability in the value.

## 08/09/2022

- Fixed the `count` verb and root datatype endpoint not obeying CensusJSON mode.

## 07/09/2022 | Update: Still Grinding
- Added the `directive`, `directive_tier`, `directive_tree` and `directive_tree_category` collections.
- Added the `directive_tier_reward` and `directive_tier_reward_set` collections.
- Squashed many bugs, introduced some more, and squashed them again.

**Notes**
⚠ NSO data is not present. I haven't unlocked an NSO character yet!
Please note that Sanctuary.Census does not know about objectives, and hence the `directive.objective_set_id` field is missing.

Further, the directive reward collections are rather different to DBG Census. Following the `directive_tier.reward_set_id` field
will require you to query the `directive_tier_reward_set` collection. You can then perform a list join on this to the
`directive_tier_reward` collection to retrieve the individual items given by the reward set.

## 03/09/2022

- Decreased the collection build time to every one hour.
- Added the `update_interval_sec` field to the `c:timing` model and datatype listing model.

## 02/09/2022

- Fixed null fields being serialized to a value of `BsonNull` when using CensusJSON mode.

## 01/09/2022

- Added the `outfit_war_match` collection.

## 31/08/2022

:warning: **Breaking Changes**
- `censusJSON` mode now defaults to true. This means that all fields will be represented as JSON strings.
  You can disable this behaviour by appending `c:censusJSON=false` to your query.
- When using `censusJSON` mode, boolean values are represented as `"0" (false)` or `"1" (true)`.
- When not using `censusJSON` mode, `int64` numbers will once again be represented as number tokens, rather than as strings.

**Additions**
- Query commands that accept a boolean value, now also support the use of `0 (false)` and `1 (true)`.
- `facility_link` and `map_region` are now updated automatically.
- Added the `map_region.localized_facility_name` field.
- Added the `map_hex` collection.

**Miscellaneous**
- Big internal refactor to collection upserting. It's now much easier to add new collections.

## 28/08/2022

- Added support for serializing all fields in a response as strings, to match DBG Census.
  Use the `c:censusJSON=true` command to achieve this.
- Updated support for outfit wars data.

## 23/08/2022

- Added support for the `c:distinct` command.
- Added the `marketing_bundle`, `marketing_bundle_category` and `marketing_bundle_item` collections.
- Added the `outfit_war_ranking` collection.

## 21/08/2022

- Fixed vehicle items missing the `faction_id` field.
- Fixed joins breaking the `c:limit` and `c:start` commands.
- Fixed inner joins not taking effect.
- Fixed non-list joins causing inner join behaviour.

## 16/08/2022

- Added the `outfit_war` and `outfit_war_rounds` collections.
- Added the `image_set` collection.
- Fixed the storage and querying of decimal (previously, floating point) values.
- Updated `item.item_category` to be nullable.
- Added the `description` and `last_updated` fields to the datatype list (i.e. `/get/<environment>`)
- Fixed querying on enum fields.
- Implemented a somewhat-correct local timezone display in the diff viewer.
- Many under the hood fixes and optimisations.

## 10/08/2022 #2

- :warning: `long/int64` numbers are now serialized as strings.
- Update timings model to include `collection_last_updated`
- `map_region` will now automatically contain accurate facility info, but still isn't automatically retrieving new regions.
- Added the `outfit_war_registration.world_id` field.
- Prevented outfit registrations from being removed if we fail to retrieve data from certain servers.

## 10/08/2022 #1

- The `facility_info` collection is now automatically updated.
- Added the `outfit_war_registration` collection.
- Added the `zone` collection.
- Added the `vehicle_skill_set` collection.

## 08/08/2022

- Added the `projectile.projectile_radius_meters` field.
- All floating point values are now rounded to 3dp.

## 07/08/2022

- Added the `projectile.proximity_lockon_range_half_meters` field.
- Added the `projectile.tether_distance` field.
- Fixed the `type` key being unusable on joins.
- Improved text wrapping in the diff viewer.

## 04/08/2022

- Fixed the use of sibling joins within child joins.
- Darkened some aspects of the diff interface.

## 30/07/2022

- Added the `facility_info` collection. This collection is currently manually updated. Compared to the current iteration of map_region,
  it is guaranteed to contain positional data and is more likely to contain all known facilities.
- Fixed querying on the `item.faction_id` field.

## 29/07/2022

- Added automatic collection diffing.
- Added a diff viewer - head to [https://census.lithafalcon.cc/diff/ps2](https://census.lithafalcon.cc/diff/ps2).
- When servers are locked, the collection build interval is reduced to 30m, so that we can get back up-to-date faster
  once the servers unlock again.

## 27/07/2022

- Added the `vehicle_attachment` and `weapon_to_attachment` collections.
- Added the `loadout` and `vehicle_loadout` collections.
- Added the `loadout_slot` and `vehicle_loadout_slot` collections.
- Increased the collection rebuild interval to three hours.
- Fixed an error when using `c:lang` and attempting to show/hide a locale field.

## 25/07/2022

- Added full support for the `c:join` command.
- Added near-full support for the `c:tree` command. The `start` key is not yet supported.
- Added support for the `c:lang` command.
- Added support for the `c:includeNull` command.
- Restricted `c:limit` to a maximum value of 10000.
- Added the `vehicle` collection.

## 22/07/2022

- The `count` verb is supported again.
- Added more informative error responses.
- `c:limitPerDB` added as an alias of `c:limit`.
- Fixed `facility_link` and `map_region` being non-queryable.
- Fixed `weapon.heat_overheat_penalty_ms` being set incorrectly

## 21/07/2022

- Added the PTS environment.
- ⚠ Temporarily removed support for the `count` verb.
- Expanded the query interface:
  - Filtering now fully supported.
  - `c:case`.
  - `c:show`.
  - `c:hide`.
  - `c:sort`.
  - `c:has`.
  - `c:timing`.
- Added the following, **manually** updated collections:
  - `facility_link`.
  - `map_region`.
- Removed automatic redirect to DBG Census for unrecognised collections.

## 17/07/2022

- Added the `weapon_to_fire_group` collection.
- Fixed the `count` verb returning incorrect values for certain collections.
- Fixed the `count` verb not redirecting to DBG Census for unknown collections.
- `fire_mode_2.recoil_angle_max` and `fire_mode_2.recoil_angle_min` are no longer nullable.

## 16/07/2022

- Added automatic redirect to DBG Census for unknown collections.
- Initial Release 🎉
