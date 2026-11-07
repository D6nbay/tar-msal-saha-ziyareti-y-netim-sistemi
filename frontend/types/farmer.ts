/* API'ın döndürdüğü farmer */
export interface Farmer{
    id: number;
    fullName: string;
    nationalId: string;
    phoneNumber: string;
    region: string;
    district: string;
    createdAt: string;
    //!! TARİHLER JSONDA METİN OLARAK GELİR
}

//Ekleyip silerken APiye giden bilgiler
export interface FarmerRequest{
    fullName: string;
    nationalId: string;
    phoneNumber: string;
    region: string;
    district: string;
}