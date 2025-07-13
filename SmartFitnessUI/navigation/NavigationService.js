// src/navigation/NavigationService.js
import { createNavigationContainerRef, CommonActions } from '@react-navigation/native';

export const navigationRef = createNavigationContainerRef();

export function resetToSignIn() {
    if (navigationRef.isReady()) {
        navigationRef.dispatch(
            CommonActions.reset({
                index: 0,
                routes: [{ name: 'SignIn' }],
            })
        );
    }
}