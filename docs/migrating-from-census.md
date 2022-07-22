# Migrating from Census

⚠ Error messages are extremely uninformative at the moment. Sorry!

## Queries

The basic structure of a query is supported. Submitting a service ID is optional, and it's not logged in any way if you do.

Only the `get` verb is supported.

Filtering is fully supported.

Supported query commands:

- `c:limit` (default `100`, max `10 000`).
- `c:limitPerDB` (overrides `c:limit`).
- `c:start`.
- `c:case`.
- `c:show`.
- `c:hide`.
- `c:sort`.
- `c:has`.
- `c:timing` - inserts a timestamp into the response detailing the length of the database query, rather than the more specific model the DBG Census inserts.

## Response Shape

Responses aim to be very similar in shape to Census. Notable differences include:

- Number, boolean and `null` values are represented appropriately, rather than as strings.
- The error response format is not consistent.

### Error Responses

Sanctuary.Census uses a different set of error codes, the definitions of which [can be found here](../Sanctuary.Census/Models/QueryErrorCode.cs).

## Collections

ℹ️ Please see the [collection model definitions here](../Sanctuary.Census/Models/Collections).

This section lists the Collections provided by Sanctuary.Census, and compares them to Census itself.
Many collections also add additional data on top of the base Census data, but this is not documented here.

⚠️ If a Census collection is not listed here, it is missing.

### 🌠 Gold Tier Collections

These collections provide the same data as their Census equivalents. The shape is not guaranteed to match, but is likely to
be very similar.

- currency
- experience
- facility_link
- faction
- fire_group
- fire_group_to_fire_mode
- fire_mode_to_projectile
- item_category
- item_to_weapon
- player_state_group_2
- ⚠ profile_2 - please query on `profile` instead, which contains all the data of this collection
- weapon_to_fire_group

### 🌟 Silver Tier Collections

These collections are missing small amounts of data as compared to their Census equivalents, or are shaped differently
in such a way that retrieving certain data may not be immediately obvious.

- item
- map_region
- projectile
- weapon
- weapon_ammo_slot
- world

#### item

Missing the `is_vehicle_weapon` and `is_default_attachment` fields. The former can be replaced by checking
if the `item_category_id` matches or inherits from **item_category** `104 - Vehicle Weapons`.

#### map_region

Missing the `reward_amount` and `reward_currency_id` fields.

#### projectile

Missing the `tether_distance` field.

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

#### profile
Missing the `movement_speed`, `backpedal_speed_modifier`, `sprint_speed_modifier` and `strafe_speed_modifier` fields.

