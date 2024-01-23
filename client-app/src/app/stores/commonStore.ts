import {ServerError} from "../models/serverError";
import {makeAutoObservable, reaction} from "mobx";

export default class CommonStore {
    error: ServerError | null = null;
    token: string | null | undefined = localStorage.getItem('jwt');
    appLoaded: boolean = false;
    
    constructor() {
        makeAutoObservable(this);
        
        reaction(
            () => this.token,
            token => {
                if (token) {
                    localStorage.setItem('jwt', token);
                } else {
                    localStorage.removeItem('jwt');
                }
            }
        )
    }
    
    setServerError(error: ServerError) {
        this.error = error;
    }
    
    setToken = (token: string | null | undefined) => {
        if(token) localStorage.setItem('jwt', token);
        this.token = token;
    }
    
    setAppLoaded = () => {
        this.appLoaded = true;
    }
}