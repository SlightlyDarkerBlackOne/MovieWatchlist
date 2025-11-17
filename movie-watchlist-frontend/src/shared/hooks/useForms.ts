import { useForm as useRHF, UseFormProps, FieldValues } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { ZodType } from 'zod';

interface UseFormHookProps<TFieldValues extends FieldValues> extends Omit<UseFormProps<TFieldValues>, 'resolver'> {
  schema: ZodType<TFieldValues>;
}

export function useForms<TFieldValues extends FieldValues = FieldValues>({
  schema,
  ...rest
}: UseFormHookProps<TFieldValues>) {
  return useRHF<TFieldValues>({
    resolver: zodResolver(schema as any) as any,
    ...rest,
  });
}


