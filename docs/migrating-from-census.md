# Migrating from Census

ℹ️ Requests to collections that Sanctuary.Census doesn't serve will be forwarded to the official Census.

## Queries

The basic structure of a query is supported. Submitting a service ID is optional, and it's not logged in any way if you do.
However, if you wish to use Sanctuary.Census as a drop-in layer, ensure you provide a service ID as it will be used
when requests are forwarded to the official Census.

Both the `get` and `count` verbs are supported.

⚠ Only the `ps2` environment is supported at this time, despite what the api-doc pages might tell you.

Note that the default value of the `c:limit` command is 100, rather than 1.

Querying itself is extremely limited at the moment:

- The only supported Census commands are `c:start` and `c:limit`.
- A collection can only be filtered on its ID field.
- A collection cannot be filtered by multiple values (e.g. `item_id=1,2`)

## Response Shape

Responses aim to be very similar in shape to Census. Notable differences include:

- Number, boolean and `null` values are represented appropriately, rather than as strings.
- The error response format is not consistent.

## Collections

ℹ️ Please see the [collection model definitions here](https://github.com/carlst99/Sanctuary.Census/tree/main/Sanctuary.Census/Models/Collections).

This section lists the Collections provided by Sanctuary.Census, and compares them to Census itself.
Many collections also add additional data on top of the base Census data, but this is not documented here.

⚠️ If a Census collection is not listed here, it is missing.

### 🌠 Gold Tier Collections

These collections provide the same data as their Census equivalents. The shape is not guaranteed to match, but is likely to
be very similar.

- currency
- experience
- faction
- fire_group
- fire_group_to_fire_mode
- fire_mode_to_projectile
- item_category
- item_to_weapon
- player_state_group_2
- profile_2 (redirects to profile)
- weapon_to_fire_group

### 🌟 Silver Tier Collections

These collections are missing small amounts of data as compared to their Census equivalents, or are shaped differently
in such a way that retrieving certain data may not be immediately obvious.

- item
- projectile
- weapon
- weapon_ammo_slot
- world

#### item

Missing the `is_vehicle_weapon` and `is_default_attachment` fields. The former can be replaced by checking
if the `item_category_id` matches or inherits from **item_category** `104 - Vehicle Weapons`.

#### projectile

Missing the `tether_distance` field.

#### weapon

Missing the `heat_capacity` field.

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

