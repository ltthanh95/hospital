namespace backend.Models
{
    public enum Gender
    {
        MALE,
        FEMALE
    }
    public enum Role
    {
        ADMIN,
        PATIENT,
        DOCTOR,
        NURSE
    }
    public enum Status
    {
        ADMISSION,
        DISCHARGE,
        ACTIVE,
        INACTIVE
    }

    public enum PaymentMethod
    {
        CASH,
        CARD,
        BANK_TRANSFER,
        INSURANCE
    }

    public enum PaymentStatus
    {
        PENDING,
        COMPLETED,
        FAILED,
        REFUNDED
    }

    public enum AppointmentStatus
    {
        PENDING,
        CONFIRMED,
        CANCELLED
    }

    public enum ChatMode
    {
        BOT,
        WAITING_FOR_DOCTOR,
        LIVE
    }

    public enum ChatSenderRole
    {
        PATIENT,
        DOCTOR,
        BOT,
        SYSTEM
    }

}
