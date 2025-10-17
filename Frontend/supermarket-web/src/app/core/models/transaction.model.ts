export interface Transaction {
  id: number;
  transactionNumber: string;
  transactionDate: Date;
  totalAmount: number;
  taxAmount: number;
  discountAmount: number;
  netAmount: number;
  paymentMethod: string;
  customerName?: string;
  customerPhone?: string;
  status: TransactionStatus;
  items: TransactionItem[];
}

export interface TransactionItem {
  productId: number;
  productName: string;
  quantity: number;
  unitPrice: number;
  discount: number;
  totalPrice: number;
}

export interface CreateTransaction {
  paymentMethod: string;
  customerName?: string;
  customerPhone?: string;
  discountAmount: number;
  items: CreateTransactionItem[];
}

export interface CreateTransactionItem {
  productId: number;
  quantity: number;
  discount: number;
}

export enum TransactionStatus {
  Pending = 0,
  Completed = 1,
  Cancelled = 2,
  Refunded = 3
}
