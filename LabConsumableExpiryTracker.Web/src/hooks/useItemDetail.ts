import { useQuery } from '@tanstack/react-query';
import { itemApi } from '../api/itemApi';

export const useItemDetail = (id?: string) => {
  return useQuery({
    queryKey: ['item', id],
    queryFn: () => itemApi.getById(id!),
    enabled: !!id,
  });
};