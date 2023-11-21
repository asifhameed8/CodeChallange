export interface User {
    UserId: number;
    FirstName?: string;
    LastName?: string;
    Username?: string;
    Password?: string;
    Token?: string;
    Role?: string;
    Email?: string;
    RefreshToken?: string;
    RefreshTokenExpiryTime: Date;
    managerId?: number;
}