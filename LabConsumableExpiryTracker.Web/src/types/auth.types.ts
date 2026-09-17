// src/types/auth.types.ts

export interface UserProfile {
  id: string;
  fullName: string;
  userName: string;
  email: string;
  isActive: boolean;
  roles: string[];
}

export interface LoginRequest {
  userName: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  tokenType: string; // ✅ Tambahkan ini (biasanya "Bearer")
  expiresAt: string; // ✅ Tambahkan ini (ISO 8601 date)
  profile: UserProfile; // ✅ Gunakan 'profile' sesuai SRS §7.2, bukan 'user'
}