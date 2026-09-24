import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { activateUser, changeUserPassword, createUser, deactivateUser, getMyProfile, getUserById, getUsers, getUserStatistics, updateMyProfile, updateUser } from "../api/userApi";
import type { ChangeUserPasswordPayload, GetUsersParams, UpdateMyProfilePayload, UpdateUserPayload } from "../types/user.types";

export const useMyProfile = () => {
  return useQuery({
    queryKey: ["my-profile"],
    queryFn: getMyProfile,
  });
};

export const useUpdateMyProfile = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (payload: UpdateMyProfilePayload) =>
      updateMyProfile(payload),

    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["my-profile"],
      });
    },
  });
};

export const useUsers = (params?: GetUsersParams) => {
    return useQuery({
        queryKey: ["users", params],
        queryFn: () => getUsers(params),
    });
};

export const useUserStatistics = () => {
    return useQuery({
        queryKey: ["users", "stats"],
        queryFn: getUserStatistics,
    });
};

export const useUser = (id?: string) => {
    return useQuery({
        queryKey: ["user", id],
        queryFn: () => getUserById(id!),
        enabled: !!id,
    });
};

export const useCreateUser = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: createUser,

        onSuccess: () => {
            queryClient.invalidateQueries({
                queryKey: ["users"],
            });
        },
    });
};

export const useUpdateUser = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: ({
            id,
            payload,
        }: {
            id: string;
            payload: UpdateUserPayload;
        }) => updateUser(id, payload),

        onSuccess: (_, variables) => {
            queryClient.invalidateQueries({
                queryKey: ["user", variables.id],
            });

            queryClient.invalidateQueries({
                queryKey: ["users"],
            });
        },
    });
};

export const useActivateUser = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (id: string) => activateUser(id),

        onSuccess: (_, id) => {
            queryClient.invalidateQueries({
                queryKey: ["user", id],
            });

            queryClient.invalidateQueries({
                queryKey: ["users"],
            });

            queryClient.invalidateQueries({
                queryKey: ["user-statistics"],
            });
        },
    });
};
export const useDeactivateUser = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: (id: string) => deactivateUser(id),

        onSuccess: (_, id) => {
            queryClient.invalidateQueries({
                queryKey: ["user", id],
            });

            queryClient.invalidateQueries({
                queryKey: ["users"],
            });

            queryClient.invalidateQueries({
                queryKey: ["user-statistics"],
            });
        },
    });
};

export const useChangeUserPassword = () => {
    const queryClient = useQueryClient();

    return useMutation({
        mutationFn: ({
            id,
            payload,
        }: {
            id: string;
            payload: ChangeUserPasswordPayload;
        }) => changeUserPassword(id, payload),

        onSuccess: (_, variables) => {
            queryClient.invalidateQueries({
                queryKey: ["user", variables.id],
            });
        },
    });
};