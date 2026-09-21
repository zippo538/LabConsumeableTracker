import { useQuery } from "@tanstack/react-query";
import { lotApi } from "../api/lotApi";

export const useLotDetail = (id?: string) => {
  return useQuery({
    queryKey: ["lot", id],
    queryFn: () => {
      if (!id) throw new Error("Lot ID is required");
      return lotApi.getLotById(id);
    },
    enabled: !!id,
  });
};

export const useLot = useLotDetail;
