export interface UserProfileDto {
  id: string;
  username: string;
  email: string;
  fullName: string;
  roles: string[];
  isActive: boolean;
}

export interface UserProfileResponse {
  success: boolean;
  message: string;
  data: UserProfileDto;
  errors: string[];
}

export interface ScientistUpdateProfileDto {
  fullName: string
}