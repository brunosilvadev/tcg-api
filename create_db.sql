-- TCG API - Initial Database Creation


-- Pindorama DB Schema
-- PostgreSQL

CREATE TYPE card_type AS ENUM ('Deity', 'Spirit', 'Creature', 'Ritual', 'Place', 'Artifact', 'Person');
CREATE TYPE card_rarity AS ENUM ('Common', 'Uncommon', 'Rare', 'Legendary');

-- Collections
CREATE TABLE collections (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name            VARCHAR(100) NOT NULL,
    slug            VARCHAR(100) NOT NULL UNIQUE,
    description     TEXT,
    total_cards     INT NOT NULL DEFAULT 0,
    is_active       BOOLEAN NOT NULL DEFAULT FALSE,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Cards
CREATE TABLE cards (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    collection_id   UUID NOT NULL REFERENCES collections(id) ON DELETE CASCADE,
    number          INT NOT NULL,
    name            VARCHAR(100) NOT NULL,
    type            card_type NOT NULL,
    rarity          card_rarity NOT NULL,
    flavor_text     TEXT,
    art_url         VARCHAR(500),
    artist_credit   VARCHAR(100),
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (collection_id, number)
);

-- Users
CREATE TABLE users (
    id                      UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email                   VARCHAR(255) NOT NULL UNIQUE,
    username                VARCHAR(50) NOT NULL UNIQUE,
    password_hash           VARCHAR(255) NOT NULL,
    booster_packs_available INT NOT NULL DEFAULT 0,
    gems                    INT NOT NULL DEFAULT 0,
    login_streak            INT NOT NULL DEFAULT 0,
    last_login_date         DATE,
    created_at              TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- User card ownership
CREATE TABLE user_cards (
    id                UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id           UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    card_id           UUID NOT NULL REFERENCES cards(id) ON DELETE CASCADE,
    quantity          INT NOT NULL DEFAULT 1 CHECK (quantity > 0),
    first_obtained_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (user_id, card_id)
);

-- Booster pack open events
CREATE TABLE booster_pack_opens (
    id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id       UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    collection_id UUID NOT NULL REFERENCES collections(id),
    opened_at     TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Cards revealed per pack open
CREATE TABLE booster_pack_cards (
    id      UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    open_id UUID NOT NULL REFERENCES booster_pack_opens(id) ON DELETE CASCADE,
    card_id UUID NOT NULL REFERENCES cards(id)
);

-- AI-generated daily facts
CREATE TABLE daily_facts (
    id             UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    collection_id  UUID NOT NULL REFERENCES collections(id),
    fact_date      DATE NOT NULL,
    content        TEXT NOT NULL,
    source_prompt  TEXT,
    generated_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (collection_id, fact_date)
);

-- Waitlist
CREATE TABLE waitlist_entries (
    id           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id      UUID REFERENCES users(id) ON DELETE SET NULL,
    email        VARCHAR(255) NOT NULL UNIQUE,
    signed_up_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Daily gem tasks (one completion per task type per user per day)
CREATE TABLE user_daily_tasks (
    id             UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id        UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    task_type      VARCHAR(50) NOT NULL,
    completed_date DATE NOT NULL,
    UNIQUE (user_id, task_type, completed_date)
);

-- Refresh tokens
CREATE TABLE refresh_tokens (
    id           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id      UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token        VARCHAR(512) NOT NULL UNIQUE,
    expires_at   TIMESTAMPTZ NOT NULL,
    created_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    is_revoked   BOOLEAN NOT NULL DEFAULT FALSE
);

-- Indexes
CREATE INDEX idx_cards_collection        ON cards(collection_id);
CREATE INDEX idx_cards_rarity            ON cards(rarity);
CREATE INDEX idx_user_cards_user         ON user_cards(user_id);
CREATE INDEX idx_user_cards_card         ON user_cards(card_id);
CREATE INDEX idx_booster_opens_user      ON booster_pack_opens(user_id);
CREATE INDEX idx_booster_cards_open      ON booster_pack_cards(open_id);
CREATE INDEX idx_daily_facts_date        ON daily_facts(fact_date);
CREATE INDEX idx_daily_facts_collection  ON daily_facts(collection_id);
CREATE INDEX idx_refresh_tokens_user     ON refresh_tokens(user_id);
CREATE INDEX idx_daily_tasks_user        ON user_daily_tasks(user_id);

-- TRUNCATE users CASCADE;