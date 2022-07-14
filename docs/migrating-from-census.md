# Migrating from Census

Sanctuary.Census aims to provide as similar of an interface to Daybreak Game's Census as possible. However, particularly
when it comes to the provided collections, this isn't possible

## Collections

This section lists the Collections provided by Sanctuary.Census, and compares them to Census itself.
Many collections also add additional data on top of the base Census data, but this is not documented here.

### 🌠 Gold Tier Collections

These collections provide the same data as their Census equivalents. The shape is not guaranteed, but likely to
be very similar.

- Currency
- Experience
- Faction
- FireGroup
- World

### 🌟 Silver Tier Collections

These collections are missing small amounts of data as compared to their Census equivalents, or are shaped differently
in such a way that retrieving certain data may not be immediately obvious.

- Item
- Weapon
- World

#### Item

Missing the `is_vehicle_weapon` and `is_default_attachment` fields. The former can be replaced by checking
if the `item_category_id` matches or inherits from ItemCategory `104 - Vehicle Weapons`.

#### Weapon

Missing the `heat_capacity` field.

#### World

Missing the `state` field. This is partially replaced by the `is_locked` field.

### ⭐ Bronze Tier Collections

These collections are missing significant amounts of data, or are shaped very differently.

- fire_group_to_fire_mode
- FireMode
- weapon_ammo_slot

#### fire_group_to_fire_mode

This Census collection has been removed, in favour of a `fire_modes` array on the `FireGroup` collection model.

#### FireMode

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

#### weapon_ammo_slot

This Census collection has been removed, in favour of an `ammo_slots` array on the `Weapon` collection model.
The AmmoSlot model is missing the `refill_ammo_rate` and `refill_ammo_delay_ms` fields, and the
`weapon_slot_index` field is replaced by the index of the AmmoSlot within the `Weapon#ammo_slots` field.
