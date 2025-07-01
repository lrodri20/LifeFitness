// src/context/AuthContext.js
import React, { createContext, useReducer, useEffect } from 'react';
import AsyncStorage from '@react-native-async-storage/async-storage';
// Use require to ensure jwtDecode is available
import jwtDecode from 'jwt-decode';

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

    // On mount, read the token from storage and validate expiration
    useEffect(() => {
        const bootstrap = async () => {
            let token = null;
            try {
                token = await AsyncStorage.getItem('userToken');
                if (token) {
                    const { exp } = jwtDecode(token);
                    const now = Date.now() / 1000;
                    if (!exp || exp <= now) {
                        // Token expired
                        await AsyncStorage.removeItem('userToken');
                        token = null;
                    }
                }
            } catch (e) {
                console.error('Failed to load or validate token', e);
                token = null;
            }
            dispatch({ type: 'RESTORE_TOKEN', token });
        };
        bootstrap();
    }, []);

    const authContext = {
        /**
         * signIn: store token and update state
         */
        signIn: async ({ token }) => {
            await AsyncStorage.setItem('userToken', token);
            dispatch({ type: 'SIGN_IN', token });
        },
        /**
         * signOut: remove token and update state
         */
        signOut: async () => {
            await AsyncStorage.removeItem('userToken');
            dispatch({ type: 'SIGN_OUT' });
        },
    };

    return (
        <AuthContext.Provider value={{
            ...state,
            ...authContext,
        }}>
            {children}
        </AuthContext.Provider>
    );
}
