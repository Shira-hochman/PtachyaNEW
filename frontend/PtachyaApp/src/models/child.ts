
export interface Child {    
    childId: number;
    kindergartenId: number; 
    idNumber: string;
    birthDate: string; 
    firstName: string;
    lastName: string;
    schoolYear: string;
    phone: string;
    email: string;
    formLink?: string | null; 
    paymentId?: number | null; 
    forms?: Form[]; 
    kindergarten?: Kindergarten; 
    payment?: Payment | null;
}


export interface Form {
    formId: number;
}

export interface Kindergarten {
    kindergartenId: number;
}

export interface Payment {
    paymentId: number;
}