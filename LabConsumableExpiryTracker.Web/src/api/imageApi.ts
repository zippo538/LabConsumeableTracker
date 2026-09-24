import { apiClient } from "./authApi";
export const getImage = async (imageId: string): Promise<Blob> => {
  const response = await apiClient.get(`/Image/${imageId}`, {
    responseType: "blob",
  });
  return response.data;
};
