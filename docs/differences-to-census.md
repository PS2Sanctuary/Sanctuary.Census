# Migrating from Census

This document serves as an outline of how well Sanctuary.Census maintains parity with the official Census.
It **does not** document any additional features that Sanctuary.Census provides over the official Census.

## Queries

The basic structure of a query is supported. Submitting a service ID is optional, and it's not logged in any way if you do.

Both the `get` and `count` verbs are supported.

Filtering (searching, querying) is fully supported.

Unsupported commands:
- `c:exactMatchFirst`.
- `c:resolve`.
- `c:retry`.

Command behaviour differences:
- `c:limit` is set to `100` by default, and has a maximum value of `10 000`.
- `c:limitPerDB` simply overrides `c:limit`.
- `c:join` allows all siblings to use child joins, however, a single query is limited to 25 joins in total.
- `c:lang` allows a comma-separated list of language codes, rather than just a single one.
- `c:tree` does not support the `start` key.
- `c:timing` inserts a slightly different model.

Any other commands not listed here are fully supported and should behave exactly as they do on the official Census.

## Response Shape

The shape of the response document should be exactly the same as the official Census.

If you would prefer that responses are serialized according to JSON spec, you can add the `c:censusJSON=false` command.
All number, boolean and `null` values will then be represented appropriately, rather than as strings.

### Error Responses

Sanctuary.Census uses a different set of error codes, the definitions of which [can be found here](../Sanctuary.Census.Api/Models/QueryErrorCode.cs).

## Collections

> **Note**:
> Please see the [collection model definitions here](../Sanctuary.Census.Common/Objects/Collections).

This section lists the Collections provided by Sanctuary.Census, and compares them to the official Census.
Many collections add additional fields on top of the official Census data, and collections which the official
Census does not provide also exist, but this is not documented here. The source of truth for Sanctuary.Census'
current set of collections can be found at the root endpoint of an environment
(e.g. [https://census.lithafalcon.cc/get/ps2](https://census.lithafalcon.cc/get/ps2)).

⚠ If an official Census collection is not listed here, it not supported.

### 🌠 Gold Tier Collections

These collections provide the same fields at minimum as their DBG Census equivalents.

- ability
- currency
- directive_tier*
- directive_tree
- directive_tree_category
- experience
- experience_rank
- facility_link
- facility_type
- faction
- fire_group
- fire_group_to_fire_mode
- fire_mode_to_projectile
- image
- image_set
- image_set_default
- item
- item_category
- item_to_weapon
- map_hex
- marketing_bundle
- marketing_bundle_item
- player_state_group_2
- projectile
- resource_type
- skill
- skill_category
- skill_line
- skill_set
- vehicle_attachment
- vehicle_skill_set
- weapon_to_attachment
- weapon_to_fire_group
- zone

#### *directive_tier

The directive reward collections are rather different to DBG Census. Following the `directive_tier.reward_set_id` field
will require you to query the `directive_tier_reward_set` collection. You can then perform a list join on this to the
`directive_tier_reward` collection to retrieve the individual items given by the reward set.

### 🌟 Silver Tier Collections

These collections are missing fields that the official Census equivalents have, or are shaped differently
in such a way that retrieving certain data may not be immediately obvious.

- directive
- loadout
- map_region
- profile
- resist_info
- vehicle
- weapon
- weapon_ammo_slot
- world

#### directive

Missing the `objective_set_id` and `qualify_requirement_id` fields.

#### profile

Missing the `profile_type_description` field.

## map_region

Missing the `tick_reward` field.

#### resist_info

Missing the `description` field.

#### vehicle

Missing the `type_name` field.

#### weapon

The `heat_capacity` field has been replaced by the `heat_threshold` field on any fire modes that
are linked to the weapon.

#### weapon_ammo_slot

Missing the `refill_ammo_rate` and `refill_ammo_delay_ms` fields.

#### world

Missing the `state` field. This is partially replaced by the `is_locked` field.

### ⭐ Bronze Tier Collections

These collections are missing significant amounts of data, or are shaped very differently.

- fire_mode

#### fire_mode

Missing the follow fields:
- `damage_direct_effect_id`
- `lockon_acquire_close_ms`
- `lockon_acquire_far_ms`
- `lockon_acquire_ms`
- `lockon_angle`
- `lockon_lose_ms`
- `lockon_maintain`
- `lockon_radius`
- `lockon_range`
- `lockon_range_close`
- `lockon_range_far`
- `lockon_required`
