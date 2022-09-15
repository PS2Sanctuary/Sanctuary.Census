# Migrating from Census

This document serves as an outline of how well Sanctuary.Census maintains parity with DBG Census.

## Queries

The basic structure of a query is supported. Submitting a service ID is optional, and it's not logged in any way if you do.

Both the `get` and `count` verbs are supported.

Filtering (searching, querying) is fully supported.

Unsupported commands:
- `c:resolve`.
- `c:exactMatchFirst`.
- `c:retry`.

Command behaviour differences:
- `c:limit` is set to `100` by default, and has a maximum value of `10 000`.
- `c:limitPerDB` simply overrides `c:limit`.
- `c:join` allows all siblings to use child joins, however, a single query is limited to 25 joins in total.
- `c:lang` allows a comma-separated list of language codes.
- `c:tree` does not support the `start` key.
- `c:timing` inserts a slightly different model.

## Response Shape

The shape of the response document should be exactly the same as DBG Census.

If you would prefer that responses are serialized according to JSON spec, you can add the `c:censusJSON=false` command.
All number, boolean and `null` values will be represented appropriately, rather than as strings.

### Error Responses

Sanctuary.Census uses a different set of error codes, the definitions of which [can be found here](../Sanctuary.Census.Api/Models/QueryErrorCode.cs).
Please note that the error response format is not consistent, although it should match Census for most query-related errors.

## Collections

ℹ Please see the [collection model definitions here](../Sanctuary.Census.Common/Objects/Collections).

This section lists the Collections provided by Sanctuary.Census, and compares them to the DBG Census.
Many collections add additional data on top of the DBG Census data, and collections which DBG Census does
not provide also exist, but this is not documented here.

⚠ If a DBG Census collection is not listed here, it not supported.

### 🌠 Gold Tier Collections

These collections provide the same data as their DBG Census equivalents. The shape is not guaranteed to match,
but is likely to be very similar.

- currency
- directive_tier*
- directive_tree
- directive_tree_category
- experience
- facility_link
- faction
- fire_group
- fire_group_to_fire_mode
- fire_mode_to_projectile
- image_set
- item_category
- item_to_weapon
- map_hex
- marketing_bundle
- marketing_bundle_item
- ⚠ player_state_group - please query on `player_state_group_2` instead, which is a direct upgrade
- player_state_group_2
- ⚠ profile_2 - please query on `profile` instead, which is a direct upgrade
- projectile
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

These collections are missing small amounts of data as compared to their DBG Census equivalents, or are shaped differently
in such a way that retrieving certain data may not be immediately obvious.

- directive
- item
- loadout
- map_region
- vehicle
- weapon
- weapon_ammo_slot
- world

#### directive

Missing the `objective_set_id` and `qualify_requirement_id` fields.

#### item

Missing the `is_default_attachment` field.

#### loadout

Missing the `code_name` field. This is replaced by querying for the name of the loadout's associated profile.

#### map_region

Missing the `reward_amount` and `reward_currency_id` fields.

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
- profile

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

Of note is that lock-on parameters seem to be calculated dynamically by the game nowadays, using an unknown
algorithm. Hence, it's not surprising that these fields have been un-obtainable and you should consider
the DBG Census equivalents invalid.

#### profile

Missing the `movement_speed`, `backpedal_speed_modifier`, `sprint_speed_modifier` and `strafe_speed_modifier` fields.
