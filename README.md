Digital Wallet — Concurrency Design
Assumptions
There is a single wallet per user.
A user can log in and access their wallet.
A user can add money to or send money from their wallet.
A user can view submitted transaction status and wallet summaries.
Wallet balance changes must be atomic and consistent even when multiple requests are processed concurrently.
1. Concurrency Handling

The system uses optimistic concurrency control with SQL Server rowversion.

Wallet.RowVersion is configured as an EF Core concurrency token.
When a wallet is updated, EF Core includes the original RowVersion in the UPDATE statement.
If another request has already modified the wallet, the update affects zero rows and EF Core throws DbUpdateConcurrencyException.
When a concurrency conflict occurs, the service:
Reloads the conflicting wallet state from the database.
Discards/detaches transaction entities created during the failed attempt.
Re-runs validation against the latest wallet state.
Recalculates the balance changes.
Retries the operation.
The service retries a maximum of 3 times.
A short exponential backoff with jitter should be used between retries to reduce repeated collisions under high contention.
If all retries are exhausted, the operation returns Failed with an appropriate failure reason rather than exposing the concurrency exception to the caller.
2. Transfer Atomicity

A transfer between wallets must be treated as a single atomic operation.

For TransferOut:

The source wallet balance is updated.
The destination wallet balance is updated.
The corresponding transaction records are created.
All changes are persisted through a single SaveChangesAsync() operation.

EF Core's transaction behavior ensures that the related changes are committed together or rolled back together.

For stronger control over complex multi-wallet operations, an explicit database transaction can be used around the complete transfer workflow.

The system must never allow a state where:

The source wallet is debited but the destination wallet is not credited.
The destination wallet is credited without the corresponding source debit.
A transaction is marked successful while the associated balance update was rolled back.
3. Idempotency

The API uses an IdempotencyKey to prevent duplicate processing caused by retries, network failures, or client-side resubmission.

IdempotencyKey has a unique filtered index.
If a request with the same idempotency key has already been successfully processed, the system returns the original transaction result instead of creating another transaction.
The idempotency record and transaction state must be persisted atomically with the balance change.

For transfer operations:

The outgoing transaction uses the client-provided IdempotencyKey.
The corresponding incoming transaction uses {IdempotencyKey}:in.
This prevents the counterpart transaction from being created more than once if the transfer workflow is retried.

Idempotency must be enforced at the **