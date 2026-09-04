# Persistence

SQLite is planned for structured durable state under XDG data paths. Live meter samples and transient runtime sessions do not belong in the database.

Every schema change requires a numbered, transactional migration and upgrade tests. Resolver changes must not silently delete identities, mappings, bindings, or user personalization. Persistence is not implemented in M0.
