# Changelog

Date format: DD/MM/YYYY

## 07/08/2022

- Fixed the `type` key being unusable on joins.

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
