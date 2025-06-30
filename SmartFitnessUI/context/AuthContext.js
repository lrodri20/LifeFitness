import React, { createContext, useReducer, useEffect } from 'react';
import AsyncStorage from '@react-native-async-storage/async-storage';

export const AuthContext = createContext();

const initialState = {
    isLoading: true,    // waiting to restore token
    userToken: null,    // the JWT
};

function reducer(state, action) {
    switch (action.type) {
        case 'RESTORE_TOKEN':
            return {
                ...state,
                userToken: action.token,
                isLoading: false,
            };
        case 'SIGN_IN':
            return {
                ...state,
                userToken: action.token,
                isLoading: false,
            };
        case 'SIGN_OUT':
            return {
                ...state,
                userToken: null,
                isLoading: false,
            };
        default:
            return state;
    }
}

export function AuthProvider({ children }) {
    const [state, dispatch] = useReducer(reducer, initialState);

    // On mount, read the token from storage
    useEffect(() => {
        const bootstrap = async () => {
            let token;
            try {
                token = await AsyncStorage.getItem('userToken');
            } catch (e) {
                console.error('Failed to load token', e);
            }
            dispatch({ type: 'RESTORE_TOKEN', token });
        };
        bootstrap();
    }, []);

    const authContext = {
        signIn: async ({ token }) => {
            await AsyncStorage.setItem('userToken', token);
            dispatch({ type: 'SIGN_IN', token });
        },
        signOut: async () => {
            await AsyncStorage.removeItem('userToken');
            dispatch({ type: 'SIGN_OUT' });
        },
    };

    return (
        <AuthContext.Provider value={{ ...state, ...authContext }}>
            {children}
        </AuthContext.Provider>
    );
}
