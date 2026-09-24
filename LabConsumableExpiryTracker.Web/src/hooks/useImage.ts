import { useState, useEffect, useRef, useCallback } from "react";
import { getImage } from "../api/imageApi";

export interface UseImageResult {
  imageUrl: string | null;
  isLoading: boolean;
  error: string | null;
  refetch: () => Promise<void>;
}

export const useImage = (
  imageId: string | null | undefined,
): UseImageResult => {
  const [imageUrl, setImageUrl] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  const activeUrlRef = useRef<string | null>(null);

  const fetchImage = useCallback(async () => {
    if (activeUrlRef.current) {
      URL.revokeObjectURL(activeUrlRef.current);
      activeUrlRef.current = null;
    }

    if (!imageId) {
      setImageUrl(null);
      setIsLoading(false);
      setError(null);
      return;
    }

    setIsLoading(true);
    setError(null);

    try {
      const blobData = await getImage(imageId);
      const url = URL.createObjectURL(blobData);

      activeUrlRef.current = url;
      setImageUrl(url);
    } catch (err: any) {
      console.error("Gagal mengambil gambar:", err);
      const errorMessage = err.response?.data?.message || "Gagal memuat gambar";
      setError(errorMessage);
      setImageUrl(null);
    } finally {
      setIsLoading(false);
    }
  }, [imageId]);

  useEffect(() => {
    fetchImage();

    return () => {
      if (activeUrlRef.current) {
        URL.revokeObjectURL(activeUrlRef.current);
        activeUrlRef.current = null;
      }
    };
  }, [fetchImage]);

  return { imageUrl, isLoading, error, refetch: fetchImage };
};

