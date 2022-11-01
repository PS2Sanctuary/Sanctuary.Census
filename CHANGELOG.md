# Changelog

Date format: DD/MM/YYYY

## vNext

💥 The deprecated `outfit_war_rounds` collection has been **removed**. Please migrate to using
`outfit_war_round`, which provides the same data but in a more query/join-friendly structure.

✨ New Collections:
- `ability`
- `ability_set`
- `resource`
- `resource_type`

🔧 Re-typed `resist_info.resist_percent` from `int` to `decimal`.

## 25/10/2022

🚨 The deprecated `outfit_war_rounds` collection will be **removed** on November 1st. Please migrate to using
`outfit_war_round`, which provides the same data but in a more query/join-friendly structure.

Added the `describe` verb, which returns a result detailing the fields of the provided collection. E.g.:

> GET https://census.lithafalcon.cc/describe/ps2/fire_mode_2
```json
{
  "fire_mode_2_list": [
    {
      "name": "fire_mode_id",
      "type": "UInt32",
      "is_nullable": "false",
      "description": "The ID of the fire mode"
    },
    {
      "name": "ammo_slot",
      "type": "Byte",
      "is_nullable": "true"
    },
    ...
  ]
}
```

**fire_mode_2** - added the following fields:
- `deploy_anim_time_ms` (`ushort?`): Time time taken by the deployment animation of a relevant weapon, e.g. the Shield Recharging Device. May have additional functionality.
- `target_requirement_expression` (`string?`): An expression defining the type of target the user must be aiming at for firing to be enabled.

**projectile** - added the following field:
- `stick_to_target_requirement_expression` (`string?`): An expression defining the type of target that the projectile can stick to.

## 22/10/2022

Setup a CORS policy allowing GET requests from any domain, with any headers.

## 13/10/2022

Added the `resist_info` collection. Do note that it's missing the `description` field.

**fire_group** - added the following fields:
- `image_set_override_id` (`uint`).
- `spin_up_time_ms` (`ushort`).

⚠ Further, the `chamber_duration_ms` field has been re-typed from a `uint` to a `ushort` value.

**fire_mode_2** - added the following fields:
- `anim_kick_magnitude` (`decimal?`).
- `anim_recoil_magnitude` (`decimal?`).
- `fire_charge_minimum_ms` (`ushort?`): the minimum amount of time that a weapon such as the Scorpion or Corsair catapult must be charged for.
- `fan_angle_rotate_degrees` (`decimal?`): the degree of rotation in the XY plane (that is, the viewing plane of the user) at which fan-based pellets are rotated.
- `fan_conical_spread` (`decimal?`): the maximum distance at which fan-based pellets are spread from each other, by means of a dedicated 'cone of fire' for each pellet.

⚠ Further, the `recoil_recovery_delay_ms` field has been re-typed from a `ushort` to a `short` value.

**projectile** - added the following fields:
- `actor_definition_first_person` (`string?`): the model used to represent the projectile.
- `lifespan_detonate` (`bool`): indicates whether the projectile will detonate at the end of its lifespan.
- `TracerFrequency` and `TracerFrequencyFirstPerson` (`byte?`): the number of projectiles that must be fired before a tracer is spawned.
- `VelocityInheritScalar` (`decimal`): the magnitude of velocity that the projectile inherits from the player entity.

**Miscellaneous**
- Fixed the `loadout.code_name` field being reset to `null` when data could not be retrieved from the server.

## 16/09/2022

**fire_mode_2**
- Added the `bullet_arc_kick_angle` field (`decimal`), which is the relative pitch angle in degrees at which a bullet exits the barrel.
- Added the `cloak_after_fire_delay_ms` field (`int?`), which is the delay in milliseconds before the infiltrator's cloak ability is usable, after firing.
  Worth noting that this field likely affects more than just cloaking, but we can't determine what, if so, at this stage.
- Added the `fan_angle` field (`decimal?`), which is the angle in degrees at which pellets such as those on the Horizon, are separated by.
- Added the `fire_needs_lock` field (`bool`), which indicates whether a lock on a target is required before the fire mode can be used.
- Added the `sprint_after_fire_delay_ms` field (`int?`), which is the delay in milliseconds before sprinting can occur, after firing.

**Other**
- Added the `item.code_factory_name` field.
- Added the `loadout.code_name` field. This brings the `loadout` collection to full parity with DBG Census.
- `weapon_ammo_slot.clip_size` is now typed as signed 16-bit integer, rather than unsigned.
- Improved some collection descriptions.

## 16/09/2022

**Outfit Wars**
- Added the `outfit_war.primary_round_id` field. This allows directly joining to the `outfit_war_ranking` collection.

- Added the `outfit_war_match.round_id` field. Note that this will be null if the match does not conclusively fit within a single round.

- Added the `outfit_war_round` collection. This provides the same data as the `outfit_war_rounds` collection,
  but in a more easily queryable and joinable manner. For example, you can now join rounds directly to `outfit_war_match`.
- ⚠ The `outfit_war_rounds` collection is now deprecated and will be removed after the end of Nexus Season 1.

**Other**
- Removed the `item.is_account_scoped` field. It did not provide any useful information, given that some items can be
  both character and account scoped depending on how they were purchased.

## 15/09/2022

Added the `item.is_vehicle_weapon` field. This will be true for any items of which their category derives from `104 - Vehicle Weapons`.
And some manually added vehicle weapon categories, namely from NSO vehicles -_-.

On the `currency` collection:
- Renamed the `icon_image_set_id` field to `image_set_id`.
- Added the `icon_id` field to maintain compatibility with Census.

On the `skill`, `skill_category`, `skill_line` and `skill_set` collections:
- Fixed incorrect values for the `image_id` and `image_path` fields, in cases where entries did not have an associated image set.
- The `image_set_id` field is now nullable.

Added `image_id` and `image_path` properties to the following collections:
- `marketing_bundle`
- `marketing_bundle_category`
- `outfit_war`
- `vehicle_loadout_slot`

## 13/09/2022 | Update: Deploy the Flail

- Added the `skill`, `skill_category`, `skill_line` and `skill_set` collections.
- Added the `no_deploy_area` collection.
- Added the `image` and `image_set_default` collections.
- The [main Sanctuary.Census deployment](https://census.lithafalcon.cc/get/ps2) now has NSO directive data for the `PS2` environment.
- Added the `outfit_war_registration.outfit_war_id` and `outfit_war_ranking.outfit_war_id` fields.

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
