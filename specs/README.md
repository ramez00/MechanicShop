# Specs — Customer Self-Booking

This folder contains the functional specifications for the customer self-booking feature (SPEC-01 → SPEC-12).

## Dependency Order

```
SPEC-01  SPEC-02  SPEC-03  SPEC-09-prereq   ← Wave 1 (parallel)
                    ↓
                 SPEC-04                     ← Wave 2a
                    ↓
                 SPEC-05                     ← Wave 2b
                    ↓
                 SPEC-06                     ← Wave 2c
            ↙         ↓         ↘
       SPEC-07    SPEC-08    SPEC-10         ← Wave 3 (parallel)
            ↘         ↓         ↙
                 SPEC-09                     ← Wave 4
                    ↓
                 SPEC-11                     ← Wave 5
                    ↓
                 SPEC-12                     ← Wave 6
```

## Specs

| File | Title | Status |
|------|-------|--------|
| [SPEC-01](./SPEC-01-lock-down-staff-endpoints.md) | Lock down staff endpoints | Pending |
| [SPEC-02](./SPEC-02-customer-role-and-account-link.md) | Customer role and account link | Pending |
| [SPEC-03](./SPEC-03-smtp-email.md) | SMTP email infrastructure | Pending |
| [SPEC-04](./SPEC-04-register-customer.md) | Register customer | Pending |
| [SPEC-05](./SPEC-05-confirm-email.md) | Confirm email and resend | Pending |
| [SPEC-06](./SPEC-06-login-with-username.md) | Login with username | Pending |
| [SPEC-07](./SPEC-07-my-vehicles.md) | My vehicles API | Pending |
| [SPEC-08](./SPEC-08-bookable-services.md) | Bookable services catalog | Pending |
| [SPEC-09-prereq](./SPEC-09-prereq-cancelled-wo-fix.md) | Exclude cancelled WOs from overlap checks | Done ✓ |
| [SPEC-09](./SPEC-09-availability.md) | Availability with capacity holds | Done ✓ |
| [SPEC-10](./SPEC-10-appointment-request-aggregate.md) | AppointmentRequest aggregate | Pending |
| [SPEC-11](./SPEC-11-submit-appointment-request.md) | Submit appointment request | Pending |
| [SPEC-12](./SPEC-12-staff-review-appointments.md) | Staff review appointments | Pending |
