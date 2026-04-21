-- ============================================================
-- Pindorama — Seed Data
-- Collection 1: Tupinambá
-- ============================================================

-- Collections
INSERT INTO collections (id, name, slug, description, total_cards, is_active)
VALUES (
    'a1000000-0000-0000-0000-000000000001',
    'Tupinambá',
    'tupinamba',
    'The first collection of Pindorama, drawing from the cosmology, spirits, creatures, rituals, and material culture of the Tupinambá people of coastal Brazil.',
    36,
    TRUE
);

-- ============================================================
-- Cards — Collection 1: Tupinambá
-- ============================================================

INSERT INTO cards (collection_id, number, name, type, rarity, flavor_text) VALUES

-- Deities
('a1000000-0000-0000-0000-000000000001',  1, 'Tupã',              'Deity',    'Legendary', 'Voice of thunder across the great water, his breath shapes the storm.'),
('a1000000-0000-0000-0000-000000000001',  2, 'Monan',             'Deity',    'Rare',      'The creator who set the world ablaze — only the plea of Irin Magé stayed his hand.'),
('a1000000-0000-0000-0000-000000000001',  3, 'Guaracy',           'Deity',    'Rare',      'The sun that watches everything, forgets nothing, and rises regardless.'),
('a1000000-0000-0000-0000-000000000001',  4, 'Jacy',              'Deity',    'Rare',      'The moon who taught women to mark time — her face changes, but she is always there.'),

-- Spirits
('a1000000-0000-0000-0000-000000000001',  5, 'Anhangá',           'Spirit',   'Uncommon',  'Guardian of animals, he wanders as a white deer — to hunt him invites ruin.'),
('a1000000-0000-0000-0000-000000000001',  6, 'Iara',              'Spirit',   'Uncommon',  'She sings from the river''s surface; men who hear her never return to shore.'),
('a1000000-0000-0000-0000-000000000001',  7, 'Curupira',          'Spirit',   'Common',    'His feet face backward to confuse those who follow — protector of the forest.'),
('a1000000-0000-0000-0000-000000000001',  9, 'Uirapuru',          'Spirit',   'Common',    'To hear its song is rare. To see it is rarer still. To be seen by it — that changes a life.'),
('a1000000-0000-0000-0000-000000000001', 10, 'Caapora',           'Spirit',   'Common',    'A child''s spirit that rides a peccary through the underbrush.'),
('a1000000-0000-0000-0000-000000000001', 11, 'Boitatá',           'Spirit',   'Uncommon',  'A great serpent of living flame, blind to everything but those who destroy the land.'),

-- Creatures
('a1000000-0000-0000-0000-000000000001',  8, 'Juriti',            'Creature', 'Common',    'The mourning dove calls at dusk where someone has died — those who know, do not follow the sound.'),
('a1000000-0000-0000-0000-000000000001', 12, 'Piranha',           'Creature', 'Uncommon',  'A single drop of blood in still water. Then the water is not still.'),
('a1000000-0000-0000-0000-000000000001', 13, 'Ariranha',          'Creature', 'Common',    'The giant otter owns the river the way the jaguar owns the forest — completely, and without apology.'),
('a1000000-0000-0000-0000-000000000001', 14, 'Sucuri',            'Creature', 'Common',    'The anaconda they say is older than the river itself.'),
('a1000000-0000-0000-0000-000000000001', 15, 'Jaguar',            'Creature', 'Common',    'The forest has a heartbeat. You only hear it when the jaguar is near.'),
('a1000000-0000-0000-0000-000000000001', 16, 'Capivara',          'Creature', 'Common',    'The river''s calm witness. She has watched floods rise and empires fall from the same muddy bank.'),

-- Rituals
('a1000000-0000-0000-0000-000000000001', 17, 'Feast of the Dead', 'Ritual',   'Uncommon',  'The dead are honored with song — their names spoken so they are not forgotten.'),
('a1000000-0000-0000-0000-000000000001', 18, 'War Paint Rite',    'Ritual',   'Uncommon',  'To wear the red of urucu is to become more than a man before battle.'),
('a1000000-0000-0000-0000-000000000001', 19, 'Maracá Calling',    'Ritual',   'Common',    'The shaman shakes the sacred rattle until the spirits lean in to listen.'),
('a1000000-0000-0000-0000-000000000001', 20, 'First Hunt',        'Ritual',   'Common',    'A boy''s first kill is not eaten — it is given back to the forest.'),

-- Places
('a1000000-0000-0000-0000-000000000001', 21, 'Yvy Marã Eỹ',       'Place',    'Rare',      'The Land Without Evil — destination of every migration, promise of every shaman.'),
('a1000000-0000-0000-0000-000000000001', 22, 'Sacred Burial Mound','Place',   'Uncommon',  'The ancestors rest here. The ground remembers their names.'),
('a1000000-0000-0000-0000-000000000001', 23, 'Pajé''s Grove',      'Place',    'Uncommon',  'No one enters without permission. The trees here have been listening for centuries.'),
('a1000000-0000-0000-0000-000000000001', 24, 'Paraná River',      'Place',    'Common',    'The great river that fed, flooded, and forgave in equal measure.'),
('a1000000-0000-0000-0000-000000000001', 25, 'Guanabara Bay',     'Place',    'Common',    'The great mouth of water where the land opens wide and the tides have their say.'),
('a1000000-0000-0000-0000-000000000001', 26, 'The Maloca',        'Place',    'Common',    'The longhouse holds thirty families and one fire — everything that matters happens here.'),

-- Artifacts
('a1000000-0000-0000-0000-000000000001', 27, 'Bone Necklace',     'Artifact', 'Rare',      'Each piece came from someone who mattered. To wear it is to carry their unfinished lives.'),
('a1000000-0000-0000-0000-000000000001', 28, 'Feather Cloak',     'Artifact', 'Uncommon',  'Thousands of feathers bound into a garment that makes a man into ceremony.'),
('a1000000-0000-0000-0000-000000000001', 29, 'Tacape',            'Artifact', 'Uncommon',  'The great war club, carved from ironwood — a weapon and a statement.'),
('a1000000-0000-0000-0000-000000000001', 30, 'Urucum Paste',      'Artifact', 'Common',    'Red pigment pressed from seeds; to wear it is to declare yourself alive.'),
('a1000000-0000-0000-0000-000000000001', 31, 'Woven Hammock',     'Artifact', 'Common',    'The Tupinambá gave this to the world. Every sailor who crossed the Atlantic slept in one.'),
('a1000000-0000-0000-0000-000000000001', 32, 'Bow and Arrow',     'Artifact', 'Common',    'Not just a weapon — a promise made to the forest that you will take only what you need.'),

-- Persons
('a1000000-0000-0000-0000-000000000001', 33, 'The Pajé',          'Person',   'Rare',      'He speaks to what cannot be seen, heals what cannot be touched, and walks where others dare not.'),
('a1000000-0000-0000-0000-000000000001', 34, 'The Warrior',       'Person',   'Uncommon',  'His body is a map of battles survived — every mark a story, every scar a name.'),
('a1000000-0000-0000-0000-000000000001', 35, 'The Elder',         'Person',   'Uncommon',  'He has forgotten more about the forest than you will ever know.'),
('a1000000-0000-0000-0000-000000000001', 36, 'Kurumin',           'Person',   'Uncommon',  'The boy watches everything. One day the forest will watch back.');

-- Give all users a booster pack to open
UPDATE users SET booster_packs_available = 1;


select * from user_cards