import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { getMyProfile, updateMyProfile } from "../api/userApi";
import type { ScientistUpdateProfileDto } from "../types/user.types";

export const useMyProfile = () => {
  return useQuery({
    queryKey: ["my-profile"],
    queryFn: getMyProfile,
  });
};

export const useUpdateMyProfile = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: ScientistUpdateProfileDto) =>
      updateMyProfile(payload),

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["my-profile"],
      });
    },
  });
};