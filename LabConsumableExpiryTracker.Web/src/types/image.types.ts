export interface UseImageOptions {
  imageId: string | null | undefined;
}

export interface UseImageResult {
  imageUrl: string | null;
  isLoading: boolean;
  error: string | null;
  refetch: () => Promise<void>;
}
